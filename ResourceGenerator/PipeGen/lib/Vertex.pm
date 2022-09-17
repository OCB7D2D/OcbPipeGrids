use strict;
use warnings;
package Vertex;
use overload  '""' => "stringify";
use Math::Vector::Real;

sub Position { $_[0]->{position} }
sub Normal { $_[0]->{normal} }

sub new
{
	my $package = shift;
	my $self = bless {
		position => $_[0],
		normal => $_[1],
	}, $package;
	# Other superconstructor stuff here
	return $self;
}

sub SetNormal
{
	my ($self, $normal) = @_;
	if ($self->{normal}) {
		$self->{normal}->set($normal);
	} else {
		$self->{normal} = V(@{$normal});
	}
}

sub clone
{
	my $self = shift;
	my $clone = bless {
		position => $self->{position} ? V(@{$self->{position}}) : undef,
		normal => $self->{normal} ? V(@{$self->{normal}}) : undef,
	}, ref $self;
	return $clone;

}

sub Rotate
{
	my ($self, $rotation, $axis, $origin) = @_;
	if (defined $self->Position)
	{
		$self->Position->[0] -= $origin->[0];
		$self->Position->[1] -= $origin->[1];
		$self->Position->[2] -= $origin->[2];
		my $rot = $axis->rotate_3d($rotation, $self->Position);
		$self->Position->[0] = $rot->[0];
		$self->Position->[1] = $rot->[1];
		$self->Position->[2] = $rot->[2];
		$self->Position->[0] += $origin->[0];
		$self->Position->[1] += $origin->[1];
		$self->Position->[2] += $origin->[2];
	}
	if (defined $self->Normal)
	{
		# $self->Normal->[0] -= $origin->[0];
		# $self->Normal->[1] -= $origin->[1];
		# $self->Normal->[2] -= $origin->[2];
		my $rot = $axis->rotate_3d($rotation, $self->Normal);
		$self->Normal->[0] = $rot->[0];
		$self->Normal->[1] = $rot->[1];
		$self->Normal->[2] = $rot->[2];
		# $self->Normal->[0] += $origin->[0];
		# $self->Normal->[1] += $origin->[1];
		# $self->Normal->[2] += $origin->[2];
	}
	return $self;
}

sub IsClose
{
	return abs($_[0]->Position->[0] - $_[1]->Position->[0]) < $_[2]
		&& abs($_[0]->Position->[1] - $_[1]->Position->[1]) < $_[2]
		&& abs($_[0]->Position->[2] - $_[1]->Position->[2]) < $_[2];
}

sub stringify
{
	return sprintf("[v: %s, vn: %s]",
		$_[0]->Position, $_[0]->Normal);
}

1;
