use strict;
use warnings;
package Mesh;
use GridV2;
use GridV3;
use Face;
use Quad;
use Triangle;
use Math::Vector::Real;
use List::Util qw(min max);
use constant PI => 4 * atan2(1, 1);
use Math::Geometry::Planar qw(DistanceToLine);

sub Face { $_[0]->{faces}->[$_[1]] }
sub Faces { @{$_[0]->{faces}} }

sub new
{
	my $package = shift;
	my $self = bless {}, $package;
	# Store complex objects
	$self->{objects} = [];
	$self->{faces} = [];
	return $self;
}

sub uniq
{
	my %hash;
	my @uniq;
	foreach (@_) {
		unless (exists $hash{\$_}) {
			push @uniq, $_;
			$hash{\$_} = 1;
		}
	}
	@uniq;

}

sub Vertices { uniq map { $_->Vertices } $_[0]->Faces }
sub Positions { uniq map { $_->Position } $_[0]->Vertices }

sub MoveBy
{
	my %seen;
	my ($self, $offset) = @_;
	# Not sure why we can't use "+=" here
	# Note: doesn't even seem to work with `$_`
	foreach my $position ($self->Positions) {
		next if exists $seen{$position};
		$position->set($position + $offset);
		$seen{$position} = 1;
	}
	return $self;
}

sub MoveBy2
{
	my %seen;
	my ($self, $offset) = @_;
	# Not sure why we can't use "+=" here
	# Note: doesn't even seem to work with `$_`
	foreach my $position ($self->Positions) {
		next if exists $seen{$position};
		$position->set($position + $offset);
		$seen{$position} = 1;
	}
	return $self;
}

sub Scale
{
	my %seen;
	my ($self, $scale) = @_;
	foreach my $vertex ($self->Vertices) {
		for (my $i = 0; $i < @{$scale}; $i++) {
			next if exists $seen{$vertex};
			$vertex->Position->[$i] *= $scale->[$i];
			$vertex->Normal->[$i] *= -1 if $scale->[$i] < 0;
			$seen{$vertex} = 1;
		}
	}
	return $self;
}

sub ScaleBy
{
	my %seen;
	my ($self, $scale) = @_;
	foreach my $vertex ($self->Vertices) {
		next if exists $seen{$vertex};
		$vertex->Position->set($vertex->Position * $scale);
		$vertex->Normal->set($vertex->Normal * -1) if $scale < 0;
		$seen{$vertex} = 1;
	}
	return $self;
}

sub Rotate
{
	my %seen;
	my ($self, $rotation, $axis, $origin) = @_;
	# Not sure why we can't use "+=" here
	# Note: doesn't even seem to work with `$_`
	foreach my $vertex ($self->Vertices) {
		next if exists $seen{$vertex};
		$vertex->Rotate($rotation, $axis, $origin);
		$seen{$vertex} = 1;
	}
	return $self;
}

sub UnifyPositions
{
	my ($self, $delta) = @_;
	my @vertices = $self->Vertices;
	for (my $i = 0; $i < @vertices; $i++)
	{
		for (my $n = $i + 1; $n < @vertices; $n++)
		{
			next if abs($vertices[$n]->Position - $vertices[$i]->Position) > $delta;
			$vertices[$n]->Position->set($vertices[$i]->Position);
		}
	}
}

# Add vertices of a complex object
# Act a bit like selection of vertices
sub CopyVertices
{
	my ($self, $object) = @_;
	push @{$self->{vertices}},
		@{$object->{vertices}};
	return $object;
}

sub CopyFaces
{
	my $self = shift;
	push @{$self->{faces}},
		map { $_->Faces } @_;
	return $self;
}

sub AddFace
{
	my ($self, $face) = @_;
	my $idx = scalar(@{$self->{faces}});
	push @{$self->{faces}}, $face;
	return $self;
}

sub Box
{
	Math::Vector::Real->box(map {
		map { $_->Position } $_->Vertices
	} $_[0]->Faces);
}

# try to add a quad, we may detect that
# we must split it into triangles instead
sub AddQuad
{
	my $self = shift;

	my $quad = Quad->new(@_);
	$self->AddFace($quad);
	return $quad;

	my $t1 = Triangle->new($_[0], $_[1], $_[3]);
	my $t2 = Triangle->new($_[1], $_[2], $_[3]);
	if (0)
	{
		$self->AddFace($t1);
		$self->AddFace($t2);
		return undef;
	}
	my $N1 = $t1->GetFaceNormal();
	my $N2 = $t2->GetFaceNormal();
	# Check if normals agree to make a quad
	if (abs($N1 - $N2) <= 1e-6){
		my $quad = Quad->new(@_);
		$self->AddFace($quad);
		return $quad;
	}
	# otherwise make two triangles
	else {
		die "Normals don't agree";
	}
}

# We know that if we extrude a circle, we can
# create squares for every new pair of vertices
# We simply return a clone of the previous object
# Adds faces accordingly, then you move and rotate
sub Extrude
{
	my $self = shift;
	my ($object, $side) = @_;
	# Let the object do the work
	# This should return a clone
	return $object->Extrude($self, $side);
}

sub CopyTo
{
	my $self = shift;
	my ($object) = @_;
	# Let the object do the work
	# This should return a clone
	return $object->CopyTo($self);
}

sub clone
{
	my $self = shift;
	my $clone = bless {
		faces => [
			map { $_->clone() }
			$self->Faces
		],
		objects => [
			map { $_->clone() }
			@{$self->{objects}}
		],
	}, ref $self;
	return $clone;
}

sub Merger
{
	my ($self, $other, $dir) = @_;

	my @lines;

	foreach my $face ($self->Faces)
	{
		foreach my $line ($face->Lines)
		{
			$line->{face} = $face;
			push @lines, $line;
		}
	}

	for (my $i = 0; $i < scalar(@lines); $i++)
	{
		for (my $n = $i + 1; $n < scalar(@lines); $n++)
		{
			next if $lines[$i] == $lines[$n];
			next if $lines[$i]->{face} == $lines[$n]->{face};

			my $a = $lines[$i];
			my $b = $lines[$n];

			next if abs($a->Vertice(1)->Position - $b->Vertice(1)->Position) < 1e-12;

			my $dist = $a->IntersectLine($b);
			if (defined $dist->[0] && $dist->[2] < 1e-6)
			{
				$a->Vertice(1)->Position->set($dist->[0]);
				$b->Vertice(1)->Position->set($dist->[1]);
			}

		}
	}

}

sub Merge
{
	my ($self, $other, $dir) = @_;

	# my (@a, @b);

	foreach my $face ($self->Faces)
	{

		$face->FaceNormal;

		foreach my $a ($face->Lines)
		{
			# find lines the are "equal to direction"
			# those qualify to be shortened by intersection
			for (my $i = 0; $i < $other->Faces; $i++)
			{
				# $other->Face($i)->FaceNormal;
				foreach my $b ($other->Face($i)->Lines)
				{
					my $dist = $a->IntersectLine($b);
					if (defined $dist->[0] && $dist->[2] < 1e-3)
					{
						# die "hello";
						$a->Vertice(1)->Position->set($dist->[0]);
						$b->Vertice(1)->Position->set($dist->[1]);
						#$b->Vertice(1)->Position->set($dist->[0]);
						#$a->Vertice(1)->Position->[2] -= 0;
						# $b->Vertice(1)->Position->set($dist->[0]);
						#$b->Vertice(1)->Position->set($dist->[1]);
					}
				}
			}
		}
	}
}

sub SimplifyFaces
{
	foreach my $face ($_[0]->Faces)
	{
		$face->Compress;
	}
}

sub InvertFaceNormals
{
	my %faces; my %vertices;
	my ($self, $invert) = @_;
	foreach my $face ($self->Faces)
	{
		next if exists $faces{$face};
		$face->{'normal'} = $face->FaceNormal * -1 if $invert;
		$faces{$face} = 1;
	}
}

sub SetNormalsFromFaces
{
	my %faces;
	my ($self, $invert) = @_;
	foreach my $face ($self->Faces)
	{
		next if exists $faces{$face};
		my %vertices;
		warn "Normals from faces";
			warn "  Normal ", $face->CalcFaceNormal;
		foreach my $vertex ($face->Vertices)
		{
			#warn $face->FaceNormal;
			next if exists $vertices{$vertex};
			#if (!$invert) { $vertex->Normal->set($face->FaceNormal); }
			#else { $vertex->Normal->set($face->FaceNormal * -1); }
			$vertices{$vertex} = 1;
			#warn $face->FaceNormal, "\n";
		}
		# $face->{'normal'} *= -1 if $invert;
		$faces{$face} = 1;
	}
}

# This is ok as result is same for shared vertices
sub InterpolateNormalsFromFaces
{
	my ($self) = @_;

	my $positions = GridV3->new(0.01);

	$positions->Add(map { $_->Position } map { $_->Vertices } $self->Faces);

	my %lookup;

	foreach my $face ($self->Faces)
	{
		foreach my $vertex ($face->Vertices)
		{
			if (exists $lookup{$vertex->Position->[3]})
			{
				push @{$lookup{$vertex->Position->[3]}}, $face;
			}
			else
			{
				$lookup{$vertex->Position->[3]} = [$face]
			}
		}
	}

	foreach my $vertex (map { $_->Vertices } $self->Faces)
	{
		unless (exists $lookup{$vertex->Position->[3]})
		{
			warn "Weirdo";
			next;
		}
		my $n = V(0, 0, 0); my $count = 0;
		foreach my $fv (uniq @{$lookup{$vertex->Position->[3]}})
		{
			$n += $fv->FaceNormal;
			$count += 1;
		}
		$n /= $count if $count > 0;
		$n /= abs($n);
		$vertex->Normal->set($n);
	}

	foreach my $face ($self->Faces)
	{
		foreach my $vertex ($face->Vertices)
		{
			$#{$vertex->Position} = 2;
		}
	}
# Deduplicate vertice positions, so that afterwards
# we can check which vertex is in which faces.

	# Every line that has two other intersecting lines
#	for (my $i = 0; $i < $self->Faces; $i++)
#	{
#		my $normal = $self->Face($i)->FaceNormal;
#		foreach ($self->Face($i)->Vertices)
#		{
#			next if ($_->{'checked'});
#			$_->{'checked'} = 1;
#			my $key = V2GridKey($_->Position);
#			if (exists $grid{$key})
#			{
#				$_->{'duplicate'} = $grid{$key};
#				warn "Found duplicate $key";
#				# $_ = $grid{$key};
#			}
#			else {
#				$grid{$key} = $_;
#			}
#		}
#		#die $face;
#	}
}

# Very specific function to join two pipes
# They but have the same detail properties
sub CutoutLineIntersection
{
	my $self = shift;
	# Every line that has two other intersecting lines
	for (my $i = 0; $i < $self->Faces; $i++)
	{
		for (my $n = $i + 1; $n < $self->Faces; $n++)
		{
			my @a = $self->Face($i)->Lines;
			my @b = $self->Face($n)->Lines;
			foreach my $a (@a) {
				foreach my $b (@b) {
					if ($a->LineDistance($b) < 1e-6) {
						#$self->Face($i)->{'deleted'} = 1;
						#$self->Face($n)->{'deleted'} = 1;
					}
				}
			}
		}
	}
}

sub ToWaveFrontObj
{
	my @lines;

	my $self = shift;

	my $positions = GridV3->new(0.00001);
	my $normals = GridV3->new(0.00001);
	my $uvs = GridV2->new(0.0000001);

	$normals->Add(map { $_->Normal } map { $_->Vertices } $self->Faces);
	# $normals->Add(map { $_->FaceNormal } $self->Faces);
	$positions->Add(map { $_->Position } map { $_->Vertices } $self->Faces);

	$uvs->Add(map { $_->UVs } $self->Faces);

	push @lines, "mtllib pipe.mtl";
	push @lines, "o pipe";

	for (my $i = 0; $i < $positions->{count}; $i++)
	{
		my $v = $positions->{list}->[$i];
		push @lines, sprintf("v %g %g %g", @{$v}[0..2]);
	}

	for (my $i = 0; $i < $normals->{count}; $i++)
	{
		my $n = $normals->{list}->[$i];
		push @lines, sprintf("vn %g %g %g", @{$n}[0..2]);
	}

	for (my $i = 0; $i < $uvs->{count}; $i++)
	{
		my $n = $uvs->{list}->[$i];
		push @lines, sprintf("vt %g %g", @{$n}[0..1]);
	}

	push @lines, "usemtl pipe";

	foreach my $face ($self->Faces)
	{
		push @lines, $face->ToWaveFrontObj();
	}

	push @lines, "";
	return join("\n", @lines);
}

use List::Util qw(min max);

sub RotateUVs
{
	my ($self, $angle) = @_;
	foreach my $face ($self->Faces)
	{
		$face->RotateUVs($angle);
	}
	return $self;
}

use POSIX "fmod";

sub ANGLE
{
	#warn "Angle $_[0]";
	#return 0.2 if $_[0] > 0.999;
	return $_[0];
}

sub CylindricUVs
{
	my ($self, $axis) = @_;
	my $w = V(1e99,-1e99); # x
	my $h = V(1e99,-1e99); # y
	my @projected;
	my @b = $axis->normal_base;
	my ($u,$j,$k) = $axis->rotation_base_3d;
	my $cc = 0;
	my $dir = 0;
	foreach my $face ($self->Faces)
	{
		push @projected, [$face, my $projected = []];
		$cc++;
		#next if $cc != 2 && $cc != 3;
		warn "ADD FACE\n";
		my $lastrot = 0;
		my @faced;
		for (my $i = 0; $i < $face->Vertices; $i++)
		{
			my $v = $face->Vertice($i)->Normal;
			my $p = $face->Vertice($i)->Position;
			my $rot = atan2($v * $b[0], $v * $b[1]) / PI;
			# warn "Normal $v on $axis => ", $rot;

			if ($rot == 0)
			{

			}
			elsif ($dir == 0)
			{
				#$dir = $rot > 0 ? 1 : $rot < 0 ? -1 : 0;
			}
			elsif ($dir > 0 && $rot < 0)
			{
				#$rot += 2;
			}
			elsif ($dir < 0 && $rot > 0)
			{
				#$rot -= 2;
			}


			my $x = 1 - $p * $axis;
			my $y = $rot;

			# warn " $x $y\n";

			# my $n = $v - ($v * $u) * $u;
			# my $x = $j * $n;
			# my $y = $k * $n;
			$w->[0] = min($w->[0], $x);
			$w->[1] = max($w->[1], $x);
			$h->[0] = min($h->[0], $y);
			$h->[1] = max($h->[1], $y);
			push @faced, [$x, $y]
		}
		if (scalar(@faced) == 4)
		{
			if ($faced[0]->[1] == $faced[1]->[1])
			{
				if ($faced[2]->[1] == $faced[3]->[1])
				{
					if ($faced[0]->[1] == -1 && $faced[2]->[1] > 0)
					{
					{
						$faced[0]->[1] = 1;
						$faced[1]->[1] = 1;
					}
					}
				}
			}
		}
		push @{$projected}, @faced;
	}
	$h->[0] = -1;
	$h->[1] = 1;
	#warn "====> ", $h->[1];
	my $dx = $w->[1] - $w->[0];
	my $dy = $h->[1] - $h->[0];
	$dx = 1 if $dx == 0;
	$dy = 1 if $dy == 0;
	foreach my $projection (@projected)
	{
		$projection->[0]->{uvs} = [map {
			V(($_->[0] - $w->[0]) / $dx,
			  (ANGLE($_->[1] - $h->[0])) / $dy);
		} @{$projection->[1]}];
		warn "ADD UVs ", join(", ", @{$projection->[0]->{uvs}});
	}
	return $self;
}


sub ProjectUVs
{
	my ($self, $axis, $scale) = @_;
	$scale = 1 unless defined $scale;
	my $w = V(1e99,-1e99); # x
	my $h = V(1e99,-1e99); # y
	my @projected;
	my ($u,$j,$k) = $axis->rotation_base_3d;
	foreach my $face ($self->Faces)
	{
		push @projected, [$face, my $projected = []];
		for (my $i = 0; $i < $face->Vertices; $i++)
		{
			my $v = $face->Vertice($i)->Position;
			my $n = $v - ($v * $u) * $u;
			my $x = $j * $n;
			my $y = $k * $n;
			$w->[0] = min($w->[0], $x);
			$w->[1] = max($w->[1], $x);
			$h->[0] = min($h->[0], $y);
			$h->[1] = max($h->[1], $y);
			push @{$projected}, [$x, $y]
		}
	}
	my $dx = $w->[1] - $w->[0];
	my $dy = $h->[1] - $h->[0];
	foreach my $projection (@projected)
	{
		$projection->[0]->{uvs} = [map {
			V(($_->[0] - $w->[0]) / $dx * $scale,
			  ($_->[1] - $h->[0]) / $dy * $scale);
		} @{$projection->[1]}];
	}
	return $self;
}


sub ProjectUVs2
{
	my ($self, $axis) = @_;
	my $w = V(1e99,-1e99); # x
	my $h = V(1e99,-1e99); # y
	my @projected;
	foreach my $face ($self->Faces)
	{
		push @projected, [$face, my $projected = []];
		for (my $i = 0; $i < $face->Vertices; $i++)
		{
			my $v = $face->Vertice($i)->Position;
			my ($u,$j,$k) = $axis->($v);
			my $n = $v - ($v * $u) * $u;
			my $x = $j * $n;
			my $y = $k * $n;
			$w->[0] = min($w->[0], $x);
			$w->[1] = max($w->[1], $x);
			$h->[0] = min($h->[0], $y);
			$h->[1] = max($h->[1], $y);
			push @{$projected}, [$x, $y]
		}
	}
	my $dx = $w->[1] - $w->[0];
	my $dy = $h->[1] - $h->[0];
	foreach my $projection (@projected)
	{
		$projection->[0]->{uvs} = [map {
			V(($_->[0] - $w->[0]) / $dx,
			  ($_->[1] - $h->[0]) / $dy);
		} @{$projection->[1]}];
	}
	return $self;
}

sub UnwrapUV
{
	my ($self) = @_;
	my $seg = 1 / $self->Faces;
	for (my $i = 0; $i < $self->Faces; $i++)
	{
		$self->Face($i)->SetUV(2/8, $seg*$i, 1, $seg*$i+$seg);
	}
}

sub UnwrapUV3
{
	my ($self) = @_;
	my $seg = 1 / $self->Faces;
	for (my $i = 0; $i < $self->Faces; $i++)
	{
		$self->Face($i)->SetUV(1/8, $seg*$i, 2/8, $seg*$i+$seg);
	}
}

sub UnwrapUV2
{
	my ($self) = @_;
	my $seg = 1 / $self->Faces;
	for (my $i = 0; $i < $self->Faces; $i++)
	{
		$self->Face($i)->SetUV2(0, $seg*$i, 1/8, $seg*$i+$seg);
	}
}

sub ParseNumber
{
	my ($txt, $def) = @_;
	unless (defined $txt) {
		return $def if defined $def;
		die "Invalid Number to Parse!";
	}
	$txt =~ s/^\s+//;
	$txt =~ s/\s+$//;
	if ($txt =~ s/[Ee]([+-]?\d+)$//)
	{
		return $txt * 10**$1;
	}
	return $txt;
}

sub read
{
	my $mesh = Mesh->new;
	open(my $fh, "<", $_[1])
	  or die "could not read $_[1]: $!";
	my $re_flt = qr/[+-]?(?:\d+(?:\.\d*)?|.\d+|)(?:[Ee][+-]?\d+)?/;
	my $re_idx = qr/(?:(\d+)?\/(\d+)?\/(\d+)?)/;

	my @vertices;
	my @normals;
	my @uvs;

	while (my $line = <$fh>)
	{
		chomp($line);
		next if $line =~ m/^\s+$/;
		if ($line =~ m/^v\s+($re_flt)\s+($re_flt)\s+($re_flt)$/)
		{
			push @vertices, [ ParseNumber($1), ParseNumber($2), ParseNumber($3) ];
		}
		elsif ($line =~ m/^v\s+($re_flt)\s+($re_flt)\s+($re_flt)\s+($re_flt)$/)
		{
			push @vertices, [ ParseNumber($1), ParseNumber($2), ParseNumber($3), ParseNumber($4) ];
		}
		elsif ($line =~ m/^vt\s+($re_flt)\s+($re_flt)$/)
		{
			push @uvs, [ ParseNumber($1), ParseNumber($2) ];
		}
		elsif ($line =~ m/^vn\s+($re_flt)\s+($re_flt)\s+($re_flt)$/)
		{
			push @normals, [ ParseNumber($1), ParseNumber($2), ParseNumber($3) ];
		}
		elsif ($line =~ m/^f\s+$re_idx\s+$re_idx\s+$re_idx/)
		{
			my $triangle = Triangle->new(
				Vertex->new(V(@{$vertices[$1-1]}), V(@{$normals[$3-1]})),
				Vertex->new(V(@{$vertices[$4-1]}), V(@{$normals[$6-1]})),
				Vertex->new(V(@{$vertices[$7-1]}), V(@{$normals[$9-1]})));
			$triangle->SetUVs(
				V(@{$uvs[$2-1]}),
				V(@{$uvs[$5-1]}),
				V(@{$uvs[$8-1]}));
			$mesh->AddFace($triangle);
		}
		elsif ($line =~ m/^f\s+$re_idx\s+$re_idx\s+$re_idx\s+$re_idx/)
		{
			die "read quad";
		}
		elsif ($line =~ m/^g/)
		{
			
		}
		else
		{
			warn "unknown $line";
		}
	}
	return $mesh;
}

sub WriteObj
{
	my ($mesh, $path, $name) = @_;
	open(my $fh, ">", ${path}) or
	  die "Error opening ${path}: $!";
	print $fh "o ${name}\n" if defined $name;
	warn "Write $mesh";
	print $fh $mesh->ToWaveFrontObj();
}

sub CalcBoundingBox
{
	my ($mesh) = @_;
	my @min = (+9e99, +9e99, +9e99);
	my @max = (-9e99, -9e99, -9e99);
	foreach my $vertice ($mesh->Vertices)
	{
		for (my $i = 0; $i < 3; $i++)
		{
			$min[$i] = min($min[$i], $vertice->Position->[$i]);
			$max[$i] = max($max[$i], $vertice->Position->[$i]);
		}
	}
	return [V(@min), V(@max)];
}

1;