use strict;
use warnings;
use Vertex;
package Vertices;
use Math::Vector::Real;

sub Vertice { $_[0]->{vertices}->[$_[1]] }
sub Vertices { @{$_[0]->{vertices}} }

sub new
{
	my $package = shift;
	my $self = bless {
		vertices => [@_],
	}, $package;
	# Other superconstructor stuff here
	return $self;
}

sub AddVertex
{
	my ($self, $position) = @_;
	my $idx = scalar(@{$self->{vertices}});
	my $vertex = Vertex->new($position);
	push @{$self->{vertices}}, $vertex;
	return $vertex;
}

# sub ConnectVertex
# {
# 	my ($self, $to) = @_;
# 	my $idx = scalar(@{$self->{vertices}});
# 	push @{$self->{vertices}}, $to;
# 	return $to;
# }

sub Rotate
{
	my $self = shift;
	my ($rotation, $axis, $origin) = @_;
	# Not sure why we can't use "+=" here
	# Note: doesn't even seem to work with `$_`
	foreach my $vertex (@{$self->{vertices}}) {
		$vertex->Rotate($rotation, $axis, $origin);
	}
	return $self;
}

sub MoveBy
{
	my ($self, $offset) = @_;
	# Not sure why we can't use "+=" here
	# Note: doesn't even seem to work with `$_`
	foreach my $vertex (@{$self->{vertices}}) {
		warn "Move ", $vertex->Position, " + ", $offset, "\n";
		$vertex->Position->set($vertex->Position + $offset);
	}
	return $self;
}

sub Scale
{
	my ($self, $scale) = @_;
	foreach my $vertex (@{$self->{vertices}}) {
		for (my $i = 0; $i < @{$scale}; $i++) {
			$vertex->Position->[$i] *= $scale->[$i];
			$vertex->Normal->[$i] *= -1 if $scale->[$i] < 0;
		}
	}
	return $self;
}

sub ScaleBy
{
	my ($self, $scale) = @_;
	foreach my $vertex (@{$self->{vertices}}) {
		$vertex->Position->set($vertex->Position * $scale);
		$vertex->Normal->set($vertex->Normal * -1) if $scale < 0;
	}
	return $self;
}

sub CreateFan
{
	my $center = V(0,0,0);
	my ($self, $scale) = @_;
	# ToDo: precalc/cache center at vertices?
	foreach my $vertex ($self->Vertices) {
			$center += $vertex->Position;
	}
	# Simple average (no median stuff!?)
	$center /= scalar($self->Vertices);
	# Now scale all vertices from center
	foreach my $vertex ($self->Vertices) {
			$vertex->Position->set($center + $scale *
				($vertex->Position - $center));
	#$vertex->Normal->set();
	}
	return $self;
}

sub ScaleCentric
{
	my $center = V(0,0,0);
	my ($self, $scale) = @_;
	# ToDo: precalc/cache center at vertices?
	foreach my $vertex ($self->Vertices) {
			$center += $vertex->Position;
	}
	# Simple average (no median stuff!?)
	$center /= scalar($self->Vertices);
	# Now scale all vertices from center
	foreach my $vertex ($self->Vertices) {
			$vertex->Position->set($center + $scale *
				($vertex->Position - $center));
	#$vertex->Normal->set();
	}
	return $self;
}

sub Debug
{
	my ($self) = @_;
	foreach my $vertex ($self->Vertices)
	{
		warn sprintf("V: %s\n", $vertex);
	}
}

sub clone
{
	my $self = shift;
	my $clone = bless {
		vertices => [
			map { $_->clone() }
			@{$self->{vertices}}
		],
		loop => $self->{loop}
	}, ref $self;
	return $clone;
}

sub VectorIsClose($$$)
{
	shift if (ref $_[0]);
	return abs($_[0]->[0] - $_[1]->[0]) < $_[2]
		&& abs($_[0]->[1] - $_[1]->[1]) < $_[2]
		&& abs($_[0]->[2] - $_[1]->[2]) < $_[2];
}

sub SetPositions
{
	foreach ($_[0]->Vertices)
	{
		$_->Position->[0] = $_[1];
		$_->Position->[1] = $_[2];
		$_->Position->[2] = $_[3];
	}
}

sub CalculateFaceNormals
{
	die "not implement generic";
}

sub SmoothNormals
{
	die "smoth";
	my @uniques;
	my ($self, $delta) = @_;
	# group all vertices at the "same" position
	# this is a pretty poor algorythm (get smart!)
	for (my $i = 0; $i < $self->Vertices; $i++)
	{
		my $is_unique = 1;
		for (my $n = 0; $n < @uniques; $n++)
		{
			# For now we just use the first item as position
			# ToDo: could average it out over all added vertices
			if ($self->Vertice($i)->IsClose($uniques[$n]->[0], $delta))
			{
				push @{$uniques[$n]}, $self->Vertice($i);
				$is_unique = 0;
			}
		}
		if ($is_unique)
		{
			push @uniques, [$self->Vertice($i)];
		}
	}

	foreach my $group (@uniques)
	{
		if (@{$group} > 1)
		{
			my $avg = V(0,0,0);
			foreach my $shared (@{$group})
			{
				$avg = $avg + $shared->Normal;
			}
			warn "AVG $avg";
			# $avg /= scalar(@{$group});
			$avg = $avg->normal_base;
			foreach my $shared (@{$group})
			{
				$shared->Normal->set($avg);
			}
		}
	}

}

1;